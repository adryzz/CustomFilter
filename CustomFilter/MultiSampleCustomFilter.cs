using System.Numerics;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using CircularBuffer;

namespace CustomFilter;

[PluginName("Custom Filter (Multi-Sample)")]
public class MultiSampleCustomFilter : IPositionedPipelineElement<IDeviceReport>
{

    public FastExpression? CalcX = null;
    public FastExpression? CalcY = null;

    private CircularBuffer<Vector2> Samples;

    private string[] parameters;
    
    /// <summary>
    /// Recompiles the X and Y polynomials to a function.
    /// </summary>
    [OnDependencyLoad]
    public void Recompile()
    {
        Samples = new CircularBuffer<Vector2>(SampleCount);
        parameters = new string[2 * (SampleCount + 2)];
        parameters[0] = "x";
        parameters[1] = "y";
        parameters[2] = "mx";
        parameters[3] = "my";
        for (int i = 0; i < SampleCount; i++)
        {
            parameters[4 + i] = "x" + i;
            parameters[5 + i] = "y" + i;
        }
        
        Entity xExpr = XFunc;
        Entity yExpr = YFunc;
        try
        {
            CalcX = xExpr.Compile(parameters);
        }
        catch (Exception ex)
        {
            CalcX = ((Entity)"x").Compile(parameters);
            Log.Exception(ex);
            Log.WriteNotify("Custom Filter", "Error while compiling X polynomial! Resetting...", LogLevel.Error);
        }
        
        try
        {
            CalcY = yExpr.Compile(parameters);
        }
        catch (Exception ex)
        {
            CalcY = ((Entity)"y").Compile(parameters);
            Log.Exception(ex);
            Log.WriteNotify("Custom Filter", "Error while compiling Y polynomial! Resetting...", LogLevel.Error);
        }
        
        Log.Debug("Custom Filter", "Recompiled all functions");
    }
    
    public void Consume(IDeviceReport value)
    {
        if (value is IAbsolutePositionReport report)
        {
            var digitizer = TabletReference.Properties.Specifications.Digitizer;
            
            Complex[] values = new Complex[2 * (SampleCount + 2)];
            values[0] = report.Position.X;
            values[1] = report.Position.Y;
            values[2] = digitizer.MaxX;
            values[3] = digitizer.MaxY;

            for (int i = 0; i < SampleCount; i++)
            {
                values[3 + i] = Samples[i].X;
                values[4 + i] = Samples[i].Y;
            }
            
            //Compiled expressions return a Complex, so we need to downcast it
            Vector2 pos = report.Position;
            if (CalcX != null)
                pos.X = (float)CalcX.Call(values).Real;

            if (CalcY != null)
                pos.Y = (float)CalcY.Call(values).Real;

            Samples.PushFront(report.Position);
            report.Position = pos;
            Emit?.Invoke(report);
        }
        else
        {
            Emit?.Invoke(value);
        }
    }

    public event Action<IDeviceReport>? Emit;
    public PipelinePosition Position => PipelinePosition.PreTransform;
    
    [Property("X coordinate polynomial"), DefaultPropertyValue("x"), ToolTip(
         "A polynomial that calculates the X coordinate\n" +
         "x = The X coordinate\n" +
         "y = The Y coordinate\n" +
         "mx = Max X coordinate\n" +
         "my = Max Y coordinate")]
    public string XFunc { get; set; }
    
    [Property("Y coordinate polynomial"), DefaultPropertyValue("y"), ToolTip(
         "A polynomial that calculates the Y coordinate\n" +
         "x = The X coordinate\n" +
         "y = The Y coordinate\n" +
         "mx = Max X coordinate\n" +
         "my = Max Y coordinate")]
    public string YFunc { get; set; }

    [Property("Samples"), DefaultPropertyValue(1)]
    public int SampleCount { get; set; } = 1;
    
    [TabletReference]
    public TabletReference TabletReference { get; set; }
}
