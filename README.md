Romanesco is an arcade shooter that started as a game jam project and was later developed further with my indie collective, Raucherpause Games. 
I handled sound design and implementation, while Suzuki Space composed the music.

All sound effects were created with digital synthesizers, giving the game a consistent sonic identity across gameplay and interface. The sound set covers:

Player: shooting, projectile hits, shield and death
Enemies: shooting, movement and death
Gameplay feedback: portal, gaining and losing a streak, and ranking
Interface: button selection, login success and failure, and the leaderboard shuffle

In the original jam version, I implemented the sounds with Unity’s built-in audio tools, triggering each one from the gameplay and UI scripts and planning them in a sound chart.

Wwise integration and refactor
To learn to work with Wwise middleware, I reworked the project by replacing Unity’s audio with a full Wwise integration.

Refactored the sound manager so gameplay code posts Wwise events instead of playing clips: code reports what happened, and Wwise decides how it sounds
Set up a naming convention and a Master, Music, and SFX bus structure, replacing the Unity AudioMixer
Built a Random Container for enemy shots with pitch and volume randomization, repeat avoidance, and a playback limit
Connected the settings menu volume sliders to the buses through RTPCs
Removed the legacy audio code and unused AudioMixer references

Coming from FMOD, I mapped each concept to its equivalent as I learned:

Multi Instrument with a random playlist → Random Container
Max instances and stealing → Playback Limit
VCAs → RTPCs mapped to bus volume
Banks → SoundBanks
