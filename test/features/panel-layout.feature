Feature: Admin panel layout

  Spec 003 groups the wood panel into four modules. Spec 001 gameplay is unchanged.

  Scenario: Player actions wait for another player
    Given the panel is open
    Then bring me, bring player, and grant admin are gray
    When the admin clicks their own row
    Then that row is highlighted
    And those three actions stay gray
    When the admin clicks another connected player
    Then that row is highlighted
    And those three actions can be clicked

  Scenario: Spawn controls wait for an item
    Given the panel is open
    Then quantity, quality, and spawn are gray
    When the admin clicks an item row
    Then that row is highlighted
    And quantity, quality, and spawn can be clicked
    And the item filter stays attached to the top of the item list

  Scenario: Cheats and skill loss ignore the selection
    Given the panel is open
    Then God, Fly, Creative, and Free cam can be clicked
    And the server skill loss slider can be moved
    And neither waits for a selected player or a selected item

  Scenario: Closing the panel keeps the action console
    Given the console shows "Spawned 5 Wood."
    When the admin closes the panel and opens it again
    Then the console still shows "Spawned 5 Wood."
    And god mode, fly, creative, and free cam are unchanged
