using Unity.NetCode;

[UnityEngine.Scripting.Preserve]
public class AutoconnectBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 7979;
        CreateDefaultClientServerWorlds();
        return true;
    }
}
