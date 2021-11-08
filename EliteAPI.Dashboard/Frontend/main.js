const {app, screen, BrowserWindow, Menu, Tray} = require("electron");
const childProcess = require('child_process');
const config = require('./config')
const path = require("path");
const {window} = require("./config");

let mainWindow;
let eliteApi;
let webSocket;
let tray;
let currentlyShown = false;

function createWindow() {
    let isNewInstance = app.requestSingleInstanceLock();

    let eliteva = {
        name: "EliteVA",
        description: "VoiceAttack plugin",
        repo: "EliteAPI/EliteVA",
        icon: ""
    }

    if (!isNewInstance) {
        app.quit();
        return false;
    }

    const display = screen.getPrimaryDisplay();

    // Create browser window
    mainWindow = new BrowserWindow({
        width: config.window.width,
        height: config.window.height,
        webPreferences: {
            preload: path.join(__dirname, 'preload.js')
        },
        x: display.bounds.width - config.window.width,
        y: display.bounds.height - config.window.height - 47,
        darkTheme: true,
        fullscreenable: false,
        maximizable: false,
        closable: false,
        hasShadow: true,
        resizable: false,
        movable: false,
        minimizable: false,
        //skipTaskbar: true,
        alwaysOnTop: true,
        titleBarStyle: 'hidden',
        icon: path.join(__dirname, 'public/icon.ico')
    });

    tray = new Tray(path.join(__dirname, 'public/icon.ico'))
    tray.setToolTip('EliteAPI')
    tray.setContextMenu(Menu.buildFromTemplate([
        {
            label: 'Open EliteAPI', type: 'normal', click: () => {
                currentlyShown = false; // force reset
                showWindow();
            }
        },
        {
            label: 'Close EliteAPI', type: 'normal', click: () => {
                console.log('killing everything')
                eliteApi.kill('SIGINT');
                app.quit();

                setTimeout(() => {
                    process.exit(1);
                }, 1000)
            }
        }
    ]));

    mainWindow.loadFile(path.join(__dirname, "public/index.html"));
    mainWindow.hide();

    tray.on('click', () => {
        showWindow();
    });

    mainWindow.on('blur', () => {
        hideWindow();
    })

    app.on('second-instance', () => {
        showWindow()
    })

    return true;
}

app.on('activate', function () {
    if (BrowserWindow.getAllWindows().length === 0) {
        createWindow()
        console.log('creating window')
    }
})

app.whenReady().then(() => {
    if (!createWindow()) {
        return;
    }

    let resourcePath = __dirname

    if ("ROLLUP_WATCH" in process.env) {
        console.log('dev mode');
    } else {
        resourcePath = process.resourcesPath;
    }

    eliteApi = childProcess.spawn(path.join(resourcePath, "public/eliteapi/EliteAPI.Dashboard.exe"));

    eliteApi.stdout.setEncoding('utf-8');
    eliteApi.stdout.on('data', (data) => {
        console.log(data.trim());
    })

    eliteApi.on('close', (error) => {
        console.warn('Closed: ' + error)
    })

    eliteApi.on('disconnect', (error) => {
        console.warn('Disconnected: ' + error)
    })

    eliteApi.on('error', (error) => {
        console.error('Error: ' + error)
    })

    eliteApi.on('exit', (error) => {
        console.warn('Exit: ' + error)
    })

    eliteApi.on('message', (error) => {
        console.warn('Message: ' + error)
    })
});


function hideWindow() {
    if (currentlyShown) {
        let opacity = 1;
        const interval = setInterval(() => {
            if (opacity <= 0) {
                clearInterval(interval);
                mainWindow.hide();
                currentlyShown = false;
            }

            mainWindow.setOpacity(opacity);
            opacity -= 0.075;
        }, 1);
    }
}

function showWindow() {
    if (currentlyShown) {
        return;
    }

    mainWindow.show();
    let opacity = 0;
    const interval = setInterval(() => {
        if (opacity >= 1) {
            clearInterval(interval);
            currentlyShown = true;
        }

        mainWindow.setOpacity(opacity);
        opacity += 0.075;
    }, 1);
}
