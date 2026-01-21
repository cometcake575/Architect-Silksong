using System.Collections.Generic;

namespace Architect.Objects.Groups;

public static class InputGroup
{
    public static readonly List<(string, string)> Generic = [];
    
    public static readonly List<(string, string)> Wav = [("New Volume", "Number")];
    
    public static readonly List<(string, string)> ObjectMover = [("Extra X", "Number"), ("Extra Y", "Number"), ("Extra Rot", "Number")];
    
    public static readonly List<(string, string)> Velocity = [("New X", "Number"), ("New Y", "Number")];
    
    public static readonly List<(string, string)> FleaCounter = [("New Value", "Number")];
}