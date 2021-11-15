<div class="plugins">
    <p class="title">{title} <span class="count">{plugins.length}</span></p>
    {#each plugins as plugin}
        <Plugin plugin="{plugin}" />
    {/each}
</div>
<script>
import Plugin from "./Plugin.svelte";

    export let title;
    export let plugins;
</script>
<style lang="scss">
      @import "../variables.scss";

      .title {
          font-size: 0.9rem;
          display: flex;
          align-items: center;

          .count {
            color: $text-muted;
            margin-left: .5rem;
            font-size: .9em;
          }

          padding: .5rem 2rem;
          padding-bottom: .25rem;
      }

      .plugins {
          margin-bottom: 2rem;
      }
</style>