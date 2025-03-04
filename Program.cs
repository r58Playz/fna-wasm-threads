using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

public class FNAGame : Game
{
    public FNAGame()
    {
        GraphicsDeviceManager gdm = new GraphicsDeviceManager(this);

        // Typically you would load a config here...
        gdm.PreferredBackBufferWidth = 512;
        gdm.PreferredBackBufferHeight = 512;
        gdm.IsFullScreen = false;
        gdm.SynchronizeWithVerticalRetrace = true;
    }

    byte r = 0;
    byte g = 0;
    byte b = 0;
    DateTime lastUpdate = DateTime.UnixEpoch;
    int updateCount = 0;

    protected override void Initialize()
    {
        /* This is a nice place to start up the engine, after
		 * loading configuration stuff in the constructor
		 */
        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Load textures, sounds, and so on in here...
        base.LoadContent();
    }

    protected override void UnloadContent()
    {
        // Clean up after yourself!
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        // Run game logic in here. Do NOT render anything here!
        base.Update(gameTime);
        updateCount++;
        DateTime now = DateTime.UtcNow;
        if ((now - lastUpdate).TotalSeconds > 1.0)
        {
            Console.WriteLine($"Main loop still running at: {now}; {Math.Round(updateCount / (now - lastUpdate).TotalSeconds, MidpointRounding.AwayFromZero)} UPS");
            lastUpdate = now;
            updateCount = 0;
        }
        if (r != 255)
        {
            r++;
            return;
        }
        if (g != 255)
        {
            g++;
            return;
        }
        if (b != 255)
        {
            b++;
            return;
        }
        r = 0;
        g = 0;
        b = 0;
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render stuff in here. Do NOT run game logic in here!
        GraphicsDevice.Clear(new Color(r, g, b));
        base.Draw(gameTime);
    }
}

partial class Program
{
    private static void Main()
    {
        Console.WriteLine("Hi!");
    }

    [DllImport("Emscripten")]
    public extern static int mount_opfs();

    static FNAGame game;
    static FieldInfo RunApplication;
    public static bool firstLaunch = true;

    [JSExport]
    internal static Task PreInit()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("calling mount_opfs");
            int ret = mount_opfs();
            Console.WriteLine($"called mount_opfs: {ret}");
            if (ret != 0)
            {
                throw new Exception("Failed to mount OPFS");
            }

            Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
        });
    }

    [JSExport]
    internal static Task Init()
    {
        // Any init for the Game - usually before game.Run() in the decompilation
        game = new FNAGame();
		RunApplication = game.GetType().GetField("RunApplication", BindingFlags.NonPublic | BindingFlags.Instance);
        return Task.Delay(0);
    }

    [JSExport]
    internal static Task<bool> Cleanup()
    {
        // Any cleanup for the Game - usually after game.Run() in the decompilation
        return Task.FromResult(true);
    }

    [JSExport]
    internal static Task<bool> MainLoop()
    {
        try
        {
            game.RunOneFrame();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in MainLoop()!");
            Console.Error.WriteLine(e);
            return (Task<bool>)Task.FromException(e);
        }
        return Task.FromResult((bool)RunApplication.GetValue(game));
    }
}
