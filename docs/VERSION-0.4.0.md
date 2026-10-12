# 0.4.0 – Green magic

Enable magic under **Settings · Magic & positions**, then close the menu. The setting is saved on the Quest and defaults to off.

Aim a flat right hand at the desired icon and briefly hold still. The border glows green, becoming brighter when readiness is recognized. Then thrust the hand approximately 12 cm quickly toward the entity. Pull the hand back afterward or briefly close it. Recognition requires a directed forward movement; slow reaching, sideways movement, tracking jumps, and holding the hand extended should not switch anything. A 1.2-second cooldown also applies after activation.

Recognition operates only with the menu closed, valid hand tracking, fresh HA states, and a switchable, localized placement. It is disabled during room capture, placement, and system dialogs. Pinch controls remain available outside the brief fireball effect. No virtual hands are rendered.

The green fireball starts at the hand, flies toward the entity with a swirling trail, and breaks into a ring and sparks. The artwork was generated with the built-in Imagegen tool; Unity animates flight, rotation, trail, and impact. The switching command is sent once on recognition. The animation visualizes the command; it does not confirm a successful HA switch. Returned state and connection status remain authoritative.

Package name, signature, and token storage remain compatible. Version 0.4.0, version code 6. Install over the existing app; do not uninstall.

## Device test

Enable magic, close the menu, aim a flat right hand at a light icon, briefly hold, then thrust forward. Check target selection, palm direction, thresholds, effect size, and transparency on Quest. Also test a closed hand, walking, sideways waving, tracking loss, sensors, disabled magic, and reopening the menu. Gestures and presentation have not yet been confirmed on real Quest hardware.
