using System;
using System.Collections.Generic;
using Manager.Models;
using Manager.Utils;
using Prometheus;

namespace Manager.Services;

public class PrometheusExporterService : IMetricExporterService
{
    private readonly Gauge _managerInfo;
    private readonly Gauge _mirrorStatusEnum;

    public PrometheusExporterService()
    {
        _managerInfo = Metrics
            .CreateGauge(Constants.PrometheusInfoMetricName, "Info of Mirrors.NET Manager", new GaugeConfiguration
            {
                LabelNames = new[] { "api_version" }
            });
        _managerInfo.WithLabels(Constants.ApiVersion).Set(1);
        _mirrorStatusEnum = Metrics
            .CreateGauge(Constants.PrometheusStatusMetricName, "Status of mirrors", new GaugeConfiguration
            {
                LabelNames = new[] { "id", "status" }
            });
    }

    public void ExportMirrorState(string mirrorName, MirrorStatus mirrorState)
    {
        foreach (MirrorStatus status in Enum.GetValues(typeof(MirrorStatus)))
        {
            _mirrorStatusEnum.WithLabels(mirrorName, status.ToString()).Set(mirrorState == status ? 1 : 0);
        }
    }
}