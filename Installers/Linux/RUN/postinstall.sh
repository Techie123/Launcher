#!/bin/bash

# Check installation priviledge
if [ "$(id -u)" != "0" ]; then
    echo "Please make sure you are running this installer with sudo or as root." 1>&2
    exit 1
fi

# Check previous versions
if type "hearthstone-uninstall" > /dev/null 2>&1
then
    echo "Previous version detected, uninstalling..."
    hearthstone-uninstall
fi

# Copy launcher / game data
cp -rf HearthstoneMod/ /opt/HearthstoneMod/

# Fix permissions
usr="$SUDO_USER"
if [ -z "$usr" -a "$usr"==" " ]; then
	usr="$USERNAME"
fi
sudo chown -R "$usr" /opt/HearthstoneMod/

# Terminal commands
cp hearthstone-mod /usr/bin/hearthstone-mod
chmod +x /usr/bin/hearthstone-mod
cp hearthstone-mod-uninstall /usr/bin/hearthstone-mod-uninstall
chmod +x /usr/bin/hearthstone-mod-uninstall

# Application launcher
cp hearthstone-mod.desktop /usr/share/applications/hearthstone-mod.desktop

echo "Installation complete :D"