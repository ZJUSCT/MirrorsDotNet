using Manager.Models;

namespace Manager.Services;

public interface IMetricExporterService
{
    public void ExportMirrorState(string mirrorName, MirrorStatus mirrorState);
}