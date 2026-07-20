namespace Minimap.Extensive;

/// <summary>Entry point for a loadable extension library.</summary>
public interface IExtension
{
    void Register(IExtensionRegistry registry);
}
