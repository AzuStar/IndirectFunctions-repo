extends SceneTree

func _initialize():
	var probe_script = load("res://Smoke.cs")
	if probe_script == null:
		push_error("Cannot load compiled C# smoke fixture")
		quit(10)
		return
	var probe = probe_script.new()
	quit(probe.Run())
