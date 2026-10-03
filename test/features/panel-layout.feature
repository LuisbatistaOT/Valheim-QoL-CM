Feature: Admin panel layout

  The gameplay rules stay those of spec 001. This feature covers where the tables and the action console sit.

  Scenario: A connected player stays highlighted
    Given the panel lists Ari and Bo
    When the admin clicks Ari
    Then Ari's row is highlighted
    And Bo's row is not highlighted
    And teleport uses Ari

  Scenario: The item filter stays above its table
    Given the panel is open
    Then the item filter is on the left under the player table
    And matching items are rows under that filter
    And those rows do not cover quantity or quality

  Scenario: Closing the panel keeps the action console
    Given the console shows "Spawned 5 Wood."
    When the admin closes the panel and opens it again
    Then the console still shows "Spawned 5 Wood."
    And god mode, fly, creative, and free cam are unchanged
