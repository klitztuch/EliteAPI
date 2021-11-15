<script>
    import { onMount } from "svelte";
    import socket from "../stores/socket";
    import Button from "./Button.svelte";

    import { loadingPlugins, openPlugins } from "../stores/stores";

    let loadingPluginsState = [];
    let openPluginsState = [];

    loadingPlugins.subscribe((value) => {
        loadingPluginsState = value;
    });

    openPlugins.subscribe((value) => {
        openPluginsState = value;
    });

    export let plugin;

    let hiddenClass = "hidden";

    onMount(() => {
        setTimeout(() => {
            hiddenClass = "";
        }, 1);
    });

    function install() {
        socket.send("Plugin.Install", plugin.name);
    }

    function uninstall() {
        socket.send("Plugin.Uninstall", plugin.name);
    }
</script>

<div class="plugin-wrapper {hiddenClass}">
    <div class="plugin">
        <div class="icon">
            <img src={plugin.icon} alt="" />
            {#if openPluginsState.includes(plugin.name)}
                <span class="status" />
            {/if}
        </div>
        <div class="info">
            <p class="title">{plugin.name}</p>
            <p class="description">{plugin.description}</p>
        </div>
    </div>
    {#if !openPluginsState.includes(plugin.name)}
        {#if !plugin.isInstalled}
            <Button
                on:click={install}
                text="Install"
                isLoading={loadingPluginsState.includes(plugin.name)}
            />
        {:else if plugin.latestVersion != plugin.installedVersion}
            <Button
                on:click={install}
                text="Update"
                isLoading={loadingPluginsState.includes(plugin.name)}
            />
        {:else}
            <Button
                on:click={uninstall}
                text="Uninstall"
                type="danger"
                isLoading={loadingPluginsState.includes(plugin.name)}
            />
        {/if}
    {/if}
</div>

<style lang="scss">
    @import "../variables.scss";

    .hidden {
        opacity: 0;
        transform: translateY(10px);
    }

    .plugin-wrapper {
        transition-duration: 200ms;
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        padding: 1rem 2rem;

        &:hover {
            background-color: $background-highlight;
        }

        .plugin {
            display: flex;
            flex-direction: row;
            align-items: center;
        }

        .icon {
            position: relative;
            margin-right: 10px;

            img {
                width: 50px;
                height: 50px;
                border-radius: 5px;
            }

            .status {
                position: absolute;
                top: -1px;
                right: -1px;
                border-radius: 100px;

                width: 5px;
                height: 5px;

                background-color: $success;
            }
        }

        .info {
            .title {
                font-size: 1.2em;
                font-weight: bold;
            }

            .description {
                font-size: 0.8em;
                color: $text-muted;
            }
        }
    }
</style>
