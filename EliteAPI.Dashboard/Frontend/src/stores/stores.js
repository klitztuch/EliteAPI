import {writable} from "svelte/store";

export const connectionState = new writable("");
export const isConnected = new writable(false);
export const userProfile = new writable({});
