using System.Numerics;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace CustomFilter;

[PluginName("Custom Filter")]
public class CustomFilter : IPositionedPipelineElement<IDeviceReport>
{
    private const string TOOLTIP =  "x = The X coordinate\n" +
                                    "y = The Y coordinate\n" +
                                    "p = The pressure\n" +
                                    "tx = The tilt X component\n" +
                                    "ty = The tilt Y component\n" +
                                    "d = The hover distance\n" +
                                    "lx = The last X coordinate\n" +
                                    "ly = The last Y coordinate\n" +
                                    "lp = The last pressure\n" +
                                    "ltx = The last tilt X component\n" +
                                    "lty = The last tilt Y component\n" +
                                    "ld = The last hover distance\n" +
                                    "mx = Max X coordinate\n" +
                                    "my = Max Y coordinate\n" + 
                                    "mp = Max pressure\n" +
                                    "cx = Last computed X coordinate\n" +
                                    "cy = Last computed Y coordinate\n" +
                                    "cp = Last computed pressure\n";

    private readonly string[] variables = { "x", "y", "p", "tx", "ty", "d", "lx", "ly", "lp", "ltx", "lty", "ld", "mx", "my", "mp", "cx", "cy", "cp" };

    public FastExpression? CalcX = null;
    public FastExpression? CalcY = null;
    public FastExpression? CalcP = null;
    public FastExpression? CalcTX = null;
    public FastExpression? CalcTY = null;

    public Vector2 LastPos = Vector2.Zero;
    public uint LastP = 0;
    public Vector2 LastT = Vector2.Zero;
    public uint LastD = 0;
    public Vector2 LastComputedPos = Vector2.Zero;
    public uint LastComputedPressure = 0;

    /// <summary>
    /// Recompiles the X and Y polynomials to a function.
    /// </summary>
    [OnDependencyLoad]
    public void Recompile()
    {
        Entity xExpr = XFunc;
        Entity yExpr = YFunc;
        Entity pExpr = PFunc;
        Entity txExpr = TXFunc;
        Entity tyExpr = TYFunc;
        try
        {
            CalcX = xExpr.Compile(variables);
        }
        catch (Exception ex)
        {
            CalcX = ((Entity)"x").Compile(variables);
            Log.Exception(ex);
            Log.WriteNotify("Custom Filter", "Error while compiling X polynomial! Resetting...", LogLevel.Error);
        }

        try
        {
            CalcY = yExpr.Compile(variables);
        }
        catch (Exception ex)
        {
            CalcY = ((Entity)"y").Compile(variables);
            Log.Exception(ex);
            Log.WriteNotify("Custom Filter", "Error while compiling Y polynomial! Resetting...", LogLevel.Error);
        }

        try
        {
            CalcP = pExpr.Compile(variables);
        }
        catch (Exception ex)
        {
            CalcP = ((Entity)"p").Compile(variables);
            Log.Exception(ex);
            Log.WriteNotify("Custom Filter", "Error while compiling P polynomial! Resetting...", LogLevel.Error);
        }

        try
        {
            CalcTX = txExpr.Compile(variables);
        }
        catch (Exception ex)
        {
            CalcTX = ((Entity)"tx").Compile(variables);
            Log.Exception(ex);
            Log.WriteNotify("Custom Filter", "Error while compiling TX polynomial! Resetting...", LogLevel.Error);
        }

        try
        {
            CalcTY = tyExpr.Compile(variables);
        }
        catch (Exception ex)
        {
            CalcTY = ((Entity)"ty").Compile(variables);
            Log.Exception(ex);
            Log.WriteNotify("Custom Filter", "Error while compiling TY polynomial! Resetting...", LogLevel.Error);
        }

        Log.Debug("Custom Filter", "Recompiled all functions");
    }

    public void Consume(IDeviceReport value)
    {
        var digitizer = TabletReference.Properties.Specifications.Digitizer;
        var pen = TabletReference.Properties.Specifications.Pen;

        Vector2 pos = Vector2.Zero;
        uint pressure = 0;
        Vector2 tilt = Vector2.Zero;
        uint distance = 0;

        if (value is ITiltReport r1)
        {
            tilt = r1.Tilt;
        }

        if (value is IProximityReport r2)
        {
            distance = r2.HoverDistance;
        }

        if (value is ITabletReport report)
        {
            //Compiled expressions return a Complex, so we need to downcast it
            pos = report.Position;
            pressure = report.Pressure;

            if (CalcX != null)
                pos.X = (float)CalcX.Call(report.Position.X, report.Position.Y, report.Pressure, tilt.X, tilt.Y, distance, LastPos.X, LastPos.Y, LastP, LastT.X, LastT.Y, LastD, digitizer.MaxX, digitizer.MaxY, pen.MaxPressure, LastComputedPos.X, LastComputedPos.Y, LastComputedPressure).Real;

            if (CalcY != null)
                pos.Y = (float)CalcY.Call(report.Position.X, report.Position.Y, report.Pressure, tilt.X, tilt.Y, distance, LastPos.X, LastPos.Y, LastP, LastT.X, LastT.Y, LastD, digitizer.MaxX, digitizer.MaxY, pen.MaxPressure, LastComputedPos.X, LastComputedPos.Y, LastComputedPressure).Real;

            if (CalcP != null)
                pressure = (uint)CalcP.Call(report.Position.X, report.Position.Y, report.Pressure, tilt.X, tilt.Y, distance, LastPos.X, LastPos.Y, LastP, LastT.X, LastT.Y, LastD, digitizer.MaxX, digitizer.MaxY, pen.MaxPressure, LastComputedPos.X, LastComputedPos.Y, LastComputedPressure).Real;

            report.Pressure = pressure;
            report.Position = pos;

            value = report;
        }

        if (value is ITiltReport r3)
        {
            if (CalcTX != null)
                tilt.X = (float)CalcTX.Call(pos.X, pos.Y, pressure, tilt.X, tilt.Y, distance, LastPos.X, LastPos.Y, LastP, LastT.X, LastT.Y, LastD, digitizer.MaxX, digitizer.MaxY, pen.MaxPressure, LastComputedPos.X, LastComputedPos.Y, LastComputedPressure).Real;

            if (CalcTY != null)
                tilt.Y = (float)CalcTY.Call(pos.X, pos.Y, pressure, tilt.X, tilt.Y, distance, LastPos.X, LastPos.Y, LastP, LastT.X, LastT.Y, LastD, digitizer.MaxX, digitizer.MaxY, pen.MaxPressure, LastComputedPos.X, LastComputedPos.Y, LastComputedPressure).Real;

            r3.Tilt = tilt;
            value = r3;
        }

        if (value is IProximityReport r4)
        {
            r4.HoverDistance = distance;
            value = r4;
        }

        Emit?.Invoke(value);

        LastComputedPos = pos;
        LastComputedPressure = pressure;
    }

    public event Action<IDeviceReport>? Emit;
    public PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("X coordinate polynomial"), DefaultPropertyValue("x"), ToolTip(
         "A polynomial that calculates the X coordinate\n" + TOOLTIP)]
    public string XFunc { get; set; }

    [Property("Y coordinate polynomial"), DefaultPropertyValue("y"), ToolTip(
         "A polynomial that calculates the Y coordinate\n" + TOOLTIP)]
    public string YFunc { get; set; }

    [Property("Pressure polynomial"), DefaultPropertyValue("p"), ToolTip(
         "A polynomial that calculates the pressure\n" + TOOLTIP)]
    public string PFunc { get; set; }

    [Property("X tilt polynomial"), DefaultPropertyValue("tx"), ToolTip(
         "A polynomial that calculates the X tilt\n" + TOOLTIP)]
    public string TXFunc { get; set; }

    [Property("Y tilt polynomial"), DefaultPropertyValue("ty"), ToolTip(
         "A polynomial that calculates the Y tilt\n" + TOOLTIP)]
    public string TYFunc { get; set; }

    [TabletReference]
    public TabletReference TabletReference { get; set; }
}