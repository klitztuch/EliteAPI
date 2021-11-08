<script>
	import {onMount} from "svelte";
	import socket from "./stores/socket";
	import {plugins} from "./plugins";
	import {connectionState, isConnected} from "./stores/stores";

	let connectionStateValue;
	let isConnectedValue;

	connectionState.subscribe(value => {
		connectionStateValue = value;
	});

	isConnected.subscribe(value => {
		isConnectedValue = value;
	});

	import Header from "./components/Header.svelte";
	import Loader from "./components/Loader.svelte";
	import Plugin from "./components/Plugin.svelte";

	onMount(() => {
	 	socket.connect();
	});
</script>

<main>
	<Header/>
	{#if isConnectedValue}
		{#each plugins as plugin}
			<Plugin plugin="{plugin}"/>
		{/each}
	{:else}
		<div class="body">
			<Loader text="{connectionStateValue}" />
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
