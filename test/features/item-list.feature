Feature: Item list accuracy

  Spec 004 keeps the spec 003 modules. Selecting an item no longer fills the quantity to a full stack, and the list omits drops the player cannot pick up.

  Scenario: Selecting an item leaves the quantity at one
    Given an item row is visible
    When the admin clicks that row
    Then the quantity is 1
    And the quality is 1
    And the console names that same row

  Scenario: Search hides drops that cannot be picked up
    Given the catalog contains several prefabs whose in-game name is Bow
    And some of those prefabs have an empty icon list
    When the admin searches for bow
    Then only the prefabs with at least one icon are listed
    And two rows that share an in-game name each show their prefab name
