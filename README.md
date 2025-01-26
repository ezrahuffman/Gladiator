# Hello!
## I am assuming that you are here from my portfolio. If not give it a look [here](https://ezrahuffman.com).

## Overview
The general Idea for this project was to create an enemy that players could fight that was driven by machine learning and not "hard coded" action patterns. In order to accomplish this I used Unity's AI agents tools. 

## Training
To train the model I used the elo rating system and gave the AI rewards for certain actions, whichever agent (versions of the same model) wins gets an update to their elo rating accordingly. Some important considerations where giving the agent points for hits, for staying within a certain range (so they acutally fought) and doing nothing for blocks.

## Controls
In an attempt to make the AI agents similar to the player, I gave the agents access to the same inputs/actions that the player controller accesses. Obviosly these inputs are controlled by the model and not actual input, but I think it worked pretty well in making the agent's movements believable. I also maintained the same cooldowns across both the player and the AI agents to again make the movements of the agents more believable.

## Is this a game?
No. This is not a game. While there is a health system, and I have set up a scene to play against the agent in the past, I would not call this a game. While the project started from the idea of making something like "Blood and Sandals" (hence the Gladiator name), I have no current plans to extend this projects.

## Future Plans
I have no current plans to expand on this project, but it is always possible I come back to it at some point.
