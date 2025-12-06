using HarmonyLib;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace TheJazMaster.MoreDifficulties;

internal static class Extensions {

	private static void WarnOnDebugAssembly(ILogger logger, Assembly? assembly)
	{
		if (assembly?.IsBuiltInDebugConfiguration() == true)
			logger.LogWarning("{Assembly} was built in debug configuration - patching may fail. If it does fail, please ask that mod's developer to build it in release configuration.", assembly.GetName().Name);
	}

	public static T GetModulo<T>(this T[] arr, int index)
	{
		return arr[Mutil.Mod(index, arr.Length)];
	}

	public static T Random<T>(this List<T> list, Rand rng)
	{
		return list[rng.NextInt() % list.Count];
	}

	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
	{
		Random rnd = new Random();
		return source.OrderBy((T item) => rnd.Next());
	}

	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Rand rng)
	{
		Rand rng2 = rng;
		return source.OrderBy((T item) => rng2.NextInt());
	}

	public static bool TryPatch(
		this Harmony self,
		MethodInfo? original,
		ILogger logger,
		LogLevel problemLogLevel = LogLevel.Error,
		LogLevel successLogLevel = LogLevel.Trace,
		HarmonyMethod? prefix = null,
		HarmonyMethod? postfix = null,
		HarmonyMethod? transpiler = null,
		HarmonyMethod? finalizer = null
	)
	{
		var originalMethod = original;
		if (originalMethod == null)
		{
			logger.Log(problemLogLevel, "Could not patch method - the mod may not work correctly.\nReason: Unknown method to patch.");
#if DEBUG
			Debugger.Break();
#endif
			return false;
		}

		try
		{

			if (transpiler is not null)
				WarnOnDebugAssembly(logger, originalMethod.DeclaringType?.Assembly);
			self.Patch(originalMethod, prefix, postfix, transpiler, finalizer);
			logger.Log(successLogLevel, "Patched method {Method}.", originalMethod.FullDescription());
			return true;
		}
		catch (Exception ex)
		{
			logger.Log(problemLogLevel, "Could not patch method {Method} - the mod may not work correctly.\nReason: {Exception}", originalMethod, ex);
#if DEBUG
			Debugger.Break();
#endif
			return false;
		}
	}
}