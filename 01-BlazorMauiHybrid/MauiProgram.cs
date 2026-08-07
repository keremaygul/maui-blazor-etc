using Microsoft.Extensions.Logging;
using StockTracker.Services;

namespace StockTracker;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// 📝 BLAZOR DERSİ - DI KAYDI:
		// Servisi DI container'a ekliyoruz. Artık herhangi bir Razor component'te
		// @inject StockService StockService yazarak kullanabiliriz.
		builder.Services.AddSingleton<StockService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
