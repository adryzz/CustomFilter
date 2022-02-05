using System.Numerics;
using AngouriMath;
using AngouriMath.Core;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace CustomFilter;

[PluginName("Custom Filter")]
public class CustomFilter : IPositionedPipelineElement<IDeviceReport>
{
    public FastExpression? CalcX = null;
    public FastExpression? CalcY = null;
    
    public Vector2 LastPos = Vector2.Zero;
    
    /// <summary>
    /// Recompiles the X and Y polynomials to a function.
    /// x = The X coordinate
    /// y = The Y coordinate
    /// lx = The last X coordinate
    /// ly = The last Y coordinate
    /// </summary>
    [OnDependencyLoad]
    public void Recompile()
    {
        Entity xExpr = XFunc;
        Entity yExpr = YFunc;
        CalcX = xExpr.Compile("x", "y", "lx", "ly");
        CalcY = yExpr.Compile("x", "y", "lx", "ly");
        Log.Debug("Custom Filter", "Recompiled all functions");
    }
    
    public void Consume(IDeviceReport value)
    {
        if (value is IAbsolutePositionReport report)
        {
            //Compiled expressions return a Complex, so we need to downcast it
            Vector2 pos = report.Position;
            if (CalcX != null)
                pos.X = (float)CalcX.Call(report.Position.X, report.Position.Y, LastPos.X, LastPos.Y).Real;

            if (CalcY != null)
                pos.Y = (float)CalcY.Call(report.Position.X, report.Position.Y, LastPos.X, LastPos.Y).Real;

            LastPos = report.Position;
            report.Position = pos;
            Emit?.Invoke(report);
        }
        else
        {
            Emit?.Invoke(value);
        }
    }

    public event Action<IDeviceReport>? Emit;
    public PipelinePosition Position { get; }
    
    [Property("X coordinate equation"), DefaultPropertyValue("x"), ToolTip(
         "A polynomial that calculates the X coordinate\n" +
         "x = The X coordinate\n" +
         "y = The Y coordinate\n" +
         "lx = The last X coordinate\n" +
         "ly = The last Y coordinate")]
    public string XFunc { get; set; }
    
    [Property("Y coordinate equation"), DefaultPropertyValue("y"), ToolTip(
         "A polynomial that calculates the Y coordinate\n" +
         "x = The X coordinate\n" +
         "y = The Y coordinate\n" +
         "lx = The last X coordinate\n" +
         "ly = The last Y coordinate")]
    public string YFunc { get; set; }
}