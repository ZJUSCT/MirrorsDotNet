# Mirrors.NET

Mirrors.NET is a simple, fast, and reliable mirror management service build with .NET. It's currently serving for [ZJU's mirror site](http://mirror.zju.edu.cn).

## Architecture

```text
┌─────────────────┐    ┌─────────────────────────────────────┐
│ mirror-frontend ├─┐  │ Mirrors.NET (Manager)               │
└─────────────────┘ │  │ ┌───────────────┐  ┌──────────────┐ │
                    ├──┤►│API Controller │  │              │ │
┌─────────────────┐ │  │ ├───────────────┤  │              │ │
│ mirror-worker   ├─┤  │ │Job Scheduler  │  │              │ │
│ ┌─────────────┐ │ │  │ ├───────────────┤  │              │ │
│ │executor:    │ │ │  │ │File Index     │  │              │ │
│ │docker       │ │ │  │ └───────────────┘  │ Internal RDB │ │
│ │containers   │ │ │  │                    │              │ │
│ └─────────────┘ │ │  │ ┌───────────────┐  │              │ │
│                 │ │  │ │Logger         │  │              │ │
└─────────────────┘ │  │ ├───────────────┤  │              │ │
                    │  │ │Metric Exporter│  │              │ │
┌─────────────────┐ │  │ └───────────────┘  └──────────────┘ │
│ mirrorz-shim    ├─┘  │                                     │
└─────────────────┘    └─────────────────────────────────────┘
```

### Mirror sync routine

```text
┌────────────────┐                  ┌────────────────┐             ┌─────────────────┐
│                │                  │                │             │                 │
│    Manager     │                  │     Worker     │             │    Executor     │
│                │                  │                │             │                 │
└───────┬────────┘                  └────────┬───────┘             └────────┬────────┘
        │                                    │                              │
        │                                    │                              │
        │        Request job                 │                              │
        │◄───────────────────────────────────┤                              │
        │                                    │                              │
        │        Job payloads                │                              │
        ├───────────────────────────────────►│                              │
        │                                    │                              │
        │                                    │  Create executor container   │
        │                                    ├─────────────────────────────►│
        │        Report status periodically  │                              │
        │◄───────────────────────────────────┤                              │
        │                                    │                              │
        │◄───────────────────────────────────┤                              │
        │                                    │                              │
        │◄───────────────────────────────────┤                              │
        │                                    │  Execution done              │
        │                                    │◄─────────────────────────────┤
        │        Report done                 │  Report infos like file size │
        │◄───────────────────────────────────┤                              │
        │                                    │                              │
        │                                    │                              │
        │                                    │                              │
        │                                    │                              │
        │                                    │                              │
┌───────┴────────┐                  ┌────────┴───────┐             ┌────────┴────────┐
│                │                  │                │             │                 │
│    Manager     │                  │     Worker     │             │    Executor     │
│                │                  │                │             │                 │
└────────────────┘                  └────────────────┘             └─────────────────┘
```

#### Step by step routine

1. Manager reads mirror configuration (upstream, sync method, schedule time, etc.) from YAML files.
2. Each mirror item generates a timer in the internal job scheduler (Hangfire).
3. When the timer is triggered, a sync job will be added to the internal DB.
4. Workers request for sync jobs through API when free, and manager will then distribute an undone job in the DB.
5. Workers execute the job in containers, and then update the job status through API.
6. When job done, workers back to free state, report to manager, manager mark the job as done.

## API

Swagger documentation is available at [Docs/Swagger.json](/Docs/Swagger.json)

### `/`

ping usage.

### `/mirrors`

You can get info about all mirrors with `GET /mirrors`.

Use `GET mirrors/{id}` to get info about a specific mirror.

### `/jobs`

Interface for managing jobs.

> Jobs are created automatically by the internal scheduler in a `cron`-like manner, and manual creation can be down in the [Hangfire dashboard](#hangfire). Hence, no explicit job creation API is provided.

`GET /jobs`: Get all jobs' info.

`POST /jobs/request`: Request a new job. (used by [worker](#worker))

`PUT jobs/{jobId}`: Update a job's info. (used by [worker](#worker))

### `/webhook`

Webhooks for mirror management.

`GET /webhook`: ping usage.

`POST webhook/index/{id}`: Trig the manager to re-gen the file index.

`PATCH webhook/sync/{id}`: Force update mirror sync status.

`POST webhook/reload`: Hot reload configs.

### `/hangfire`

Internal Hangfire's dashboard.

### `/metrics`

Prometheus metrics exporting manager's basic info, mirrors' status and Hangfire's job status. An example metric is placed at [Docs/Metrics.md](/Docs/Metrics.md).

## Configs

Example configs can be found at [Manager/Configs](/Manager/Configs).

### Config Folder Example Layout

Config files should be placed with the following layout convention. The config folder is located at `/app/Configs` in the container, which can be bind with a host folder.

```text
Configs
├── SyncConfig
│   └── debian.yml
└── IndexConfig
    └── debian-iso.yml
```

## Worker

Written in Go, please refer to [mirror-worker](https://github.com/ZJUSCT/mirror-worker).
