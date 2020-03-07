# YARDT
This is a deck tracker for Legends Of Runeterra. Made primarily so that you don't have to use the Mobalytics one since it uses Overwolf.

## Setup

To use the deck tracker, you will need to do a first time launch. The deck tracker will download and then crop all of the necessary images, after which you will be free to use it. 

## Future plans

Currently the deck tracker is functioning as intended, feel free to add suggestions (via pull request)

List of features planned:

 - [x] Port change option
 - [x] Automatic updates
 - [x] Expedition support
 - [x] Card stats (cards left in deck/hand, draw chances)
 - [x] Support for different languages
 - [ ] Enemy deck
 - [ ] Enemy cards in hand
 - [ ] Card graveyard
 - [ ] Darken colour of cards that can no longer be drawn (0 of that card left in deck)
 - [ ] Add shuffled cards to deck tracker
 - [ ] Deck winrate stats
 - [ ] Auto minimize deck tracker when not in game

## Known issues

The way the program is set up now is not 100% efficient, but that is due to Riot's API not functioning properly on Expeditions; The static decklist of the api often returns errors when used during expeditions, so a workaround had to be implemented. It will be changed once Riot's API no longer produces the errors.

## Design

You can see the design of the deck tracker below, feedback greatly appreciated!

![](https://i.imgur.com/8nobIgy.png)
![](https://i.imgur.com/bKfAuS1.png)
