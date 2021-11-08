import {connectionState, isConnected} from "./stores";

export default {
    socket: null,

    connect() {
        this.socket = new WebSocket("ws://localhost:5000/ws", 'EliteAPI-app');
        connectionState.set("Connecting to EliteAPI");
        isConnected.set(false);

        this.socket.onmessage = (e) => {

        }

        this.socket.onclose = (e) => {
            isConnected.set(false);
            connectionState.set("Connection closed");
            setTimeout(() => {
                this.connect();
            }, 5000)
        }

        this.socket.onopen = (e) => {
            isConnected.set(true);
            connectionState.set("Connected established");
            send(this.socket, 'auth', 'frontend')
        }

        this.socket.onerror = (e) => {
            isConnected.set(false);
            connectionState.set("Could not connect to EliteAPI");
        }
    },
}

function send(socket, type, data) {
    let message = {
        type: type,
        value: data
    };

    let compressed = compress(message);
    socket.send(compressed);
}

function compress(message) {
    return JSON.stringify(message);
}

function decompress(message) {
    let object = JSON.parse(message);

    if (object.value && (object.value.startsWith("{") || object.value.startsWith("["))) {
        object.value = JSON.parse(object.value);
    }

    return object;
}
