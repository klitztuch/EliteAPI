<script>
	import { onMount } from "svelte";
	import socket from "./stores/socket";
	import { connectionState, isConnected, userProfile } from "./stores/stores";

	let connectionStateValue;
	let isConnectedState;
	let userProfileState = {};

	let availablePlugins = [];
	let installedPlugins = [];
	let runningPlugins = [];

	connectionState.subscribe((value) => {
		connectionStateValue = value;
	});

	isConnected.subscribe((value) => {
		isConnectedState = value;
	});

	userProfile.subscribe((value) => {
		userProfileState = value;

		if (userProfileState.plugins) {
			console.log("SETTING", userProfileState);
			installedPlugins = userProfileState.plugins.filter(
				(x) => x.isInstalled === true
			);
			availablePlugins = userProfileState.plugins.filter(
				(x) => x.isInstalled === false
			);
		}
	});

	import Header from "./components/Header.svelte";
	import Loader from "./components/Loader.svelte";
	import Plugin from "./components/Plugin.svelte";
import PluginGroup from "./components/PluginGroup.svelte";

	onMount(() => {
		socket.connect();
	});
</script>

<main>
	<Header />
	{#if isConnectedState}
		<div class="plugins">
			<PluginGroup title="Installed" plugins="{installedPlugins}" />
			<PluginGroup title="Available" plugins="{availablePlugins}" />
		</div>
	{:else}
		<div class="body">
			<Loader text={connectionStateValue} />
		</div>
	{/if}
</main>

<style lang="scss">
	@import "./variables.scss";

	main {
		background-color: $background;
		color: $text;
		border: $border;
		display: flex;
		flex-direction: column;

		.body {
			display: flex;
			flex-grow: 1;
			padding: 20px;
			flex-direction: column;
			align-items: center;
			justify-content: center;
		}
	}
</style>
