﻿using mauiapp.ViewModels;
using Microsoft.Extensions.Logging;

namespace mauiapp;

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
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<IRestService, RestService>()
						.AddSingleton<MainPage>()
						.AddSingleton<MainPageViewModel>()
						.AddTransient<CTCPage>()
						.AddTransient<CTCPageViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}