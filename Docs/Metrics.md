# Metrics

This is an example output of the returned metrics, which can be referenced when configuring queries or panels in Grafana.

```text
# HELP dotnet_total_memory_bytes Total known allocated memory
# TYPE dotnet_total_memory_bytes gauge
dotnet_total_memory_bytes 25255888
# HELP process_working_set_bytes Process working set
# TYPE process_working_set_bytes gauge
process_working_set_bytes 130203648
# HELP manager_info Info of Mirrors.NET Manager
# TYPE manager_info gauge
manager_info{api_version="v2"} 1
# HELP process_open_handles Number of open handles
# TYPE process_open_handles gauge
process_open_handles 0
# HELP dotnet_collection_count_total GC collection count
# TYPE dotnet_collection_count_total counter
dotnet_collection_count_total{generation="0"} 0
dotnet_collection_count_total{generation="1"} 0
dotnet_collection_count_total{generation="2"} 0
# HELP process_private_memory_bytes Process private memory size
# TYPE process_private_memory_bytes gauge
process_private_memory_bytes 0
# HELP process_start_time_seconds Start time of the process since unix epoch in seconds.
# TYPE process_start_time_seconds gauge
process_start_time_seconds 1659261893.662215
# HELP hangfire_job_count Number of Hangfire jobs
# TYPE hangfire_job_count gauge
hangfire_job_count{state="retry"} 0
hangfire_job_count{state="succeeded"} 0
hangfire_job_count{state="scheduled"} 0
hangfire_job_count{state="failed"} 0
hangfire_job_count{state="processing"} 0
hangfire_job_count{state="enqueued"} 2
# HELP mirror_status Status of mirrors
# TYPE mirror_status gauge
mirror_status{id="debian",status="Failed"} 0
mirror_status{id="debian",status="Cached"} 0
mirror_status{id="debian-cd",status="Succeeded"} 0
mirror_status{id="debian-cd",status="Unknown"} 0
mirror_status{id="ubuntu",status="Unknown"} 1
mirror_status{id="debian",status="Paused"} 0
mirror_status{id="ubuntu",status="ReverseProxied"} 0
mirror_status{id="debian",status="Succeeded"} 0
mirror_status{id="debian-cd",status="Failed"} 0
mirror_status{id="debian",status="Unknown"} 1
mirror_status{id="ubuntu",status="Failed"} 0
mirror_status{id="ubuntu",status="Succeeded"} 0
mirror_status{id="debian-cd",status="ReverseProxied"} 0
mirror_status{id="debian",status="Syncing"} 0
mirror_status{id="debian",status="ReverseProxied"} 0
mirror_status{id="debian-cd",status="Syncing"} 0
mirror_status{id="ubuntu",status="Paused"} 0
mirror_status{id="ubuntu",status="Cached"} 0
mirror_status{id="ubuntu",status="Syncing"} 0
mirror_status{id="debian-cd",status="Cached"} 1
mirror_status{id="debian-cd",status="Paused"} 0
# HELP process_cpu_seconds_total Total user and system CPU time spent in seconds.
# TYPE process_cpu_seconds_total counter
process_cpu_seconds_total 0.0605748
# HELP process_num_threads Total number of threads
# TYPE process_num_threads gauge
process_num_threads 41
# HELP process_virtual_memory_bytes Virtual memory size in bytes.
# TYPE process_virtual_memory_bytes gauge
process_virtual_memory_bytes 443337490432
```