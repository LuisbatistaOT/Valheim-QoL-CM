Feature: Nearby actions

  Spec 005 adds Ghost, Tame, and Kill enemies to Global Cheats. Version 1.5.

  Scenario: Ghost toggles on the admin
    Given the admin is alive
    When the admin clicks Ghost
    Then ghost mode flips on that character
    And God, Fly, Creative, and Free cam stay as they were
    And the button reads Ghost: On or Ghost: Off

  Scenario: Tame and Kill enemies run on the host
    Given the admin is alive
    When the admin clicks Tame
    Then the host tames the creatures the vanilla tame command would tame
    And the console reports the count
    When the admin clicks Kill enemies
    Then the host kills untamed enemies within 1000 of that admin
    And players and tamed creatures stay alive
    And the console reports the count

  Scenario: A dead admin changes nothing
    Given the admin is dead
    When the admin clicks Ghost, Tame, or Kill enemies
    Then the console says Character is dead.
    And ghost mode, creatures, and enemies stay as they were
