# unitygame-tower-defense
A small game project I'm working on with my kid

## Roadmap

### First Playable

* (DONE) Towers attack enemies
  * (DONE) Towers can be pre-placed in map scenes
  * (DONE) Towers attack enemies if in range
  * (DONE) Towers lead their target based on their projectile speed
  * (DONE) Towers can be placed by the player
* (DONE) Enemies spawn in predetermined waves of specified types
  * (DONE) Add slow basic enemy
  * (DONE) Add fast basic enemy
  * (DONE) Enemy waves can co-exist in time without affecting each other
* (DONE) Enemy health and damage to it
  * (DONE) Enemies have health (amount defined per enemy)
  * (DONE) Click-shooter script causes damage to enemies
  * (DONE) Tower projectiles cause damage to enemies
* (DONE) Player health and damage to it
  * (DONE) Player has health
  * (DONE) Enemies cause player damage (amount defined per enemy) if they get through their path undefeated
* Money as resource
  * (DONE) Game begins with initial funds
  * (DONE) Towers cost money to place
  * (DONE) Defeating enemies gives money instantly
  * Prize at the end of a mission based on enemies defeated and preset reward
  * Restrictions on placing Towers
    * (DONE) Towers have cooldowns for placement
    * Towers can only be placed on predetermined areas, as in not on enemy paths
* Add Main Menu scene
  * Just a simple "Press any key to start"
  * Needs music
  * Needs sound effects
* More added later...

## Dependencies

Some of the 3rd party assets used are not attainable via Package Manager, so they're listed below

### Unity AssetStore
* Futuristic Gun SoundFX by MGWSoundDesign: Tower sounds
  https://assetstore.unity.com/packages/audio/sound-fx/weapons/futuristic-gun-soundfx-100851
* Laser Turret by ursa.anim
  https://assetstore.unity.com/packages/3d/props/guns/laser-turret-36177
* Surge by Pixelplacement: Used to create spline paths in the game
  https://assetstore.unity.com/packages/tools/utilities/surge-107312
* Toon Muzzleflash Pack by David Stenfors
  https://assetstore.unity.com/packages/2d/textures-materials/toon-muzzleflash-pack-56572

### OpenGameArt.org
* Animated Characters 1 by Kenney
  https://opengameart.org/content/animated-characters-1
* Animated Characters 2 by Kenney
  https://opengameart.org/content/animated-characters-2
