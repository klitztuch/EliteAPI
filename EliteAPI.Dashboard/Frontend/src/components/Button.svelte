<script>
    import Spinner from "./Spinner.svelte";

    export let text;
    export let type;
    export let isLoading;
</script>

<button on:click class={type}>
    {#if !isLoading}
        {text}
    {:else}
        <Spinner />
    {/if}
</button>

<style lang="scss">
    @import "../variables.scss";

    button {
        border: none;
        cursor: pointer;
        outline: none;
        transition: all 0.2s ease-in-out;
        background-color: $accent;

        display: flex;
        align-items: center;
        justify-content: center;
        text-align: center;

        padding: 0.25rem 1rem;
        color: $text-accent;

        border: $border;
        border-radius: 0.25rem;

        min-width: 80px;

        &:hover {
            background-color: $accent-light;
        }
    }

    .danger {
        background-color: $background;
        border-color: $danger;
        color: $danger;

        &:hover {
            background-color: $background-highlight;
        }
    }
</style>
