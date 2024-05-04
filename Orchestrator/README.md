# Mirror Manager (Orchestrator)

The mirror manager service for [ZJU Mirror](https://mirrors.zju.edu.cn/).

## Quick Start

1. Build app image by `docker build -t mirror-manager Orchestrator/`
2. Launch service by `docker-compose.yml`
3. Add sync configs in `/app/Configs`. The config json schema is located at `Orchestrator/Schemas/mirror-item.schema.json`
4. Modify token secrets in `/app/appsettings.json`
5. Hack into source code at `http://{webaddr}/swagger`

## Design Docs

*Orchestrator* acts like a central controller for the mirror system. The service updates its state passively, which means it relies on a worker request to process the job queue. Therefore, parallel job running depends on the number of workers.

All sync item will be assigned exactly one job, pushed to the job queue. Job syncing can be described as two parts:

- Job fetch: The manager first checks for timeout jobs, mark them as failed, and push to the queue. Then it dequeues a job, checks if interval is passed after last run, dispatch it to the worker or push it back to the queue. The dispatched job will be added to a dict for tracing.
- Status update: The worker reports job result, successful or failed. A new job for the sync item will be pushed to the job queue.

Note: The manager will save mirror status and other info to database, but jobs are not saved.

## API Endpoints

- **Jobs**: Communicating with workers, job list for monitoring, force running a sync job without interval limit
- **Mirrors**: Mirror list and details
- **MirrorZ**: API Endpoint for [MirrorZ Project](https://mirrorz.org/)
- **Webhook**: Dynamic update sync items while running

For details, launch *orchestrator* and visit Swagger UI.
