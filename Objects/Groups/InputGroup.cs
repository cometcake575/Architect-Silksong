using System.Collections.Generic;
using Architect.Events.Blocks;

namespace Architect.Objects.Groups;

public static class InputGroup
{
    public static readonly List<(string, string)> Generic = [];
    
    public static readonly List<(string, string)> Wav = [("New Volume", "Number")];
    
    public static readonly List<(string, string)> ObjectMover = [("Extra X", "Number"), ("Extra Y", "Number"), ("Extra Rot", "Number")];
    
    public static readonly List<(string, string)> Velocity = [("New X", "Number"), ("New Y", "Number")];
    
    public static readonly List<(string, string)> FleaCounter = [("New Value", "Number")];

    public static readonly List<(string, string)> Png =
    [
        ("New Width", "Number"),
        ScriptBlock.Space,
        ("New Height", "Number"),
        ScriptBlock.Space,
        ("New FPS", "Number")];
    
    public static readonly List<(string, string)> Colourer = [
        ("R", "Number"), 
        ("G", "Number"),
        ("B", "Number"), 
        ("A", "Number"),
        ("Fade Time", "Number")
    ];
}