<script>
</script>

<div class="header">
    <div class="eliteapi">
        <img src="icon.png" alt="" />
        <h1>EliteAPI</h1>
    </div>
    <div class="settings">
        <p>settings</p>
    </div>
</div>

<style lang="scss">
    @import "../variables.scss";

    .header {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        border-bottom: $border;

        padding-left: 2rem;
        padding-right: 2rem;

        .eliteapi {
            display: flex;
            flex-direction: row;
            align-items: center;

            img {
                height: 4rem;
                padding-top: 1rem;
                padding-bottom: 1rem;
                padding-right: 1rem;
            }
        }

        background-color: $background-accent;
        color: $text-accent;

        h1 {
            font-size: 1.4rem;
        }
    }
</style>
