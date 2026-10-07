Feature: Defects and diagnostic log

  Spec 006 keeps Kill enemies, makes a saved skill-loss percent of 0 survive relog, and opens the panel with +. Version 1.6.

  Scenario: Kill enemies reports a miss
    Given the admin is alive
    When the admin clicks Kill enemies and no enemy qualifies
    Then the console says No enemy was nearby.
    And the debug log records the admin position, radius 1000, how many characters were seen, and that none were killed

  Scenario: Zero skill loss survives relog
    Given the admin applies 0% skill loss
    When the admin leaves and joins again
    Then the slider and the percent box show 0%
    And the next death removes no skill level
    And gear still drops

  Scenario: Plus opens the panel
    Given the saved binding is backtick or Ctrl+Tab
    When the plugin loads
    Then the binding is +
    And + opens and closes the panel
    And backtick does not
