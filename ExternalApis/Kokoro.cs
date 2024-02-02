using System;
using System.Diagnostics.CodeAnalysis;

namespace TheJazMaster.MoreDifficulties;

public partial interface IKokoroApi
{
	void RegisterTypeForExtensionData(Type type);
	T GetExtensionData<T>(object o, string key);
	bool TryGetExtensionData<T>(object o, string key, [MaybeNullWhen(false)] out T data);
	T ObtainExtensionData<T>(object o, string key, Func<T> factory);
	T ObtainExtensionData<T>(object o, string key) where T : new();
	bool ContainsExtensionData(object o, string key);
	void SetExtensionData<T>(object o, string key, T data);
	void RemoveExtensionData(object o, string key);

	IActionApi Actions { get; }

	public interface IActionApi
	{
		CardAction MakeExhaustEntireHandImmediate();
		CardAction MakePlaySpecificCardFromAnywhere(int cardId, bool showTheCardIfNotInHand = true);
		CardAction MakePlayRandomCardsFromAnywhere(IEnumerable<int> cardIds, int amount = 1, bool showTheCardIfNotInHand = true);

		CardAction MakeContinue(out Guid id);
		CardAction MakeContinued(Guid id, CardAction action);
		IEnumerable<CardAction> MakeContinued(Guid id, IEnumerable<CardAction> action);
		CardAction MakeStop(out Guid id);
		CardAction MakeStopped(Guid id, CardAction action);
		IEnumerable<CardAction> MakeStopped(Guid id, IEnumerable<CardAction> action);

		CardAction MakeHidden(CardAction action, bool showTooltips = false);
		AVariableHint SetTargetPlayer(AVariableHint action, bool targetPlayer);
		AVariableHint MakeEnergyX(AVariableHint? action = null, bool energy = true, int? tooltipOverride = null);
		AStatus MakeEnergy(AStatus action, bool energy = true);

		List<CardAction> GetWrappedCardActions(CardAction action);
		List<CardAction> GetWrappedCardActionsRecursively(CardAction action);
		List<CardAction> GetWrappedCardActionsRecursively(CardAction action, bool includingWrapperActions);

		void RegisterWrappedActionHook(IWrappedActionHook hook, double priority);
		void UnregisterWrappedActionHook(IWrappedActionHook hook);
	}
}

public interface IWrappedActionHook
{
	List<CardAction>? GetWrappedCardActions(CardAction action);
}