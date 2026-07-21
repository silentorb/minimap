@tool
extends EditorPlugin

## Godot 4.6 EmbedSizeMode.SIZE_MODE_STRETCH — Game bar "Stretch to Fit".
const EMBED_SIZE_MODE_STRETCH := 2

func _enter_tree() -> void:
	EditorSettings.get_singleton().set_project_metadata(
		"game_view",
		"embed_size_mode",
		EMBED_SIZE_MODE_STRETCH
	)
