# 0.3.2 – Hand controls, placement, and deletion

- Room capture no longer blocks the entire menu while waiting for its callback. After returning from the Quest system dialog, the menu repositions in front of the head. Missing callbacks have a timeout. Hand pointers use tracked hands without the previous global high-confidence barrier; system gestures remain excluded.
- Icons sit on a dark circle with a turquoise border matching the menu. Between 1.2 m and 30 cm, they smoothly grow to 1.8 times their size. Name and value still appear only within 30 cm.
- Default placement: point and pinch once, review the preview, then save or continue moving. Choose a distance between 0.5 and 2 m beforehand. Direct hand placement also requires only one pinch, not a prolonged hold. Hand position is smoothed and the preview remains small.
- Placed-entity management/deletion in the main menu also lists entities no longer available in Home Assistant. Deletion does not require a localized room anchor. The mapping is removed persistently first; separate system-anchor cleanup cannot undo deletion. Failed cleanup may leave an unused system anchor.
- Package name, signature, and protected credentials remain update-compatible. Version 0.3.2, version code 5. Install over the existing app; do not uninstall.

## Quest checks

1. Edit the room map, finish the system dialog, then use the right hand for menus and entities. Also test cancellation.
2. Approach an icon: check the circular background, border, and continuous enlargement; text appears only within 30 cm.
3. Place an entity with the pointer: pinch, move again, and confirm. Also test direct hand placement with a single pinch.
4. Delete a visible placement and an unlocalized placement through management. Restart the app and confirm both remain deleted.
5. Install the update over 0.3.1 and check automatic login.

These changes have not yet been confirmed on real Quest hardware.
