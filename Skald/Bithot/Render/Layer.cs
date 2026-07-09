namespace Skald.Bithot.Render;

public enum LayerGroup
{
	Core,
	Tectonics,
}

public record VisualLayer(
	string Id,
	LayerGroup Group,
	int PriorityWithinGroup,
	Action<float> RenderAction
);

public class LayerManager
{
	private readonly List<VisualLayer> _registeredLayers = [];
	private readonly HashSet<string> _activeLayerIds = [];
	private const float StepMargin = 0.002f;

	public void Register(VisualLayer layer) => _registeredLayers.Add(layer);

	public void SetLayerActive(string id, bool active)
	{
		if (active) _activeLayerIds.Add(id);
		else _activeLayerIds.Remove(id);
	}

	public void RebuildScene(float baseVisualRadius)
	{
		var sortedActive = _registeredLayers
			.Where(l => _activeLayerIds.Contains(l.Id))
			.OrderBy(l => l.Group)
			.ThenBy(l => l.PriorityWithinGroup)
			.ToList();

		for (var i = 0; i < sortedActive.Count; i++)
		{
			var targetRadius = baseVisualRadius * (1.0f + (i * StepMargin));
			sortedActive[i].RenderAction(targetRadius);
		}
	}
}
