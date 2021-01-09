# SimpleStashie

Plugin for ExileApi https://github.com/Queuete/ExileApi

Auto clicker to dump your inventory into your Stash. Sorting needs to be done by the Ingame implemented Stash Affinities.
Minimal configuration:

* `IgnoredCells` -> Inventory cells which should NOT be dumped to Stash.
* `StashItKey` -> Hotkey which starts the process.
* `WaitTimeInMs` -> The time which the program waits between clicks. A too low setting may cause getting kicked from the server due to "too many actions".
* `AmountOfRetries` -> There are some cases where the first click wont transfer the item. This specifies how often your inventory is checked for Items to transfer.
