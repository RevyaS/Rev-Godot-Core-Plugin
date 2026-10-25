using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;
using MessagePack;

/// <summary>
/// Autoload over <see cref="MessagePackData"/>. Relative paths resolve against <see cref="RootPath"/> (res:// by default),
/// paths that already name a root (res://, user://, absolute) are used as they are.
/// </summary>
public partial class RevDataManager : Node
{
    public const string DefaultRootPath = "res://";

    public string RootPath { get; set; } = DefaultRootPath;

    public static RevDataManager Get(Node node) => node.GetTree().Root.GetNode<RevDataManager>(nameof(RevDataManager));

    public string Resolve(string path) => path.IsAbsolutePath() ? path : RootPath.PathJoin(path);

    public void Save<T>(string path, T data, MessagePackSerializerOptions? options = null) => MessagePackData.Save(Resolve(path), data, options);

    public T Load<T>(string path, MessagePackSerializerOptions? options = null) => MessagePackData.Load<T>(Resolve(path), options);

    public bool TryLoad<T>(string path, [MaybeNullWhen(false)] out T data, MessagePackSerializerOptions? options = null)
        => MessagePackData.TryLoad(Resolve(path), out data, options);

    public bool Exists(string path) => MessagePackData.Exists(Resolve(path));

    public void Delete(string path) => MessagePackData.Delete(Resolve(path));

    public SaveSlotLayout ResolveLayout(SaveSlotLayout layout) => layout with { Directory = Resolve(layout.Directory) };

    public IReadOnlyList<SaveSlot> GetSlots(SaveSlotLayout layout) => ResolveLayout(layout).GetSlots();
}
