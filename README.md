# Mirrors.NET

Mirrors.NET is a simple, fast, and reliable mirror management service build with .NET. It's currently serving for [ZJU's mirror site](http://mirrors.zju.edu.cn).

## Architecture

### Overall

### Manager

#### API

TBD

#### Configs

Example:

```text
Configs
├── SyncConfig
│   └── debian.yml
└── IndexConfig
    └── debian-iso.yml
```

### Worker

Written in Go, please refer to [mirror-worker](https://github.com/ZJUSCT/mirror-worker)
