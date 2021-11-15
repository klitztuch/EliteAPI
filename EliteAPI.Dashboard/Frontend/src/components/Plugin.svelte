<script>
    import { onMount } from "svelte";
    import socket from "../stores/socket";

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
</script>

<div class="plugin-wrapper {hiddenClass}">
    <div class="plugin">
        <img src={plugin.icon} alt="" />
        <div class="info">
            <p class="title">{plugin.name}</p>
            <p class="description">{plugin.description}</p>
        </div>
    </div>
    {#if !plugin.isInstalled}
        <button on:click={install}>Install</button>
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

        img {
            width: 50px;
            height: 50px;
            margin-right: 10px;
            border-radius: 5px;
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
