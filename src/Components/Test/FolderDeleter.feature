@FolderDeleter
Feature: FolderDeleter
	In order not to delete large folders by mistake
	I want to use a special folder deleter component

Background:
	Given I have a folder beneath the user's temp folder but not immediately recognizable as such

Scenario: Simple small folder can be deleted
	Given the folder contains 10 files
	When I ask the folder deleter if I am allowed to delete the folder
	And I ask the folder deleter to double-check this
	Then the result is yes
	And the double-check agrees
	When I ask the folder deleter to delete the folder
	Then the folder is gone

Scenario: Simple large folder cannot be deleted
	Given the folder contains 20 files
	When I ask the folder deleter if I am allowed to delete the folder
	And I ask the folder deleter to double-check this
	Then the result is no
	And the double-check agrees
	When I ask the folder deleter to delete the folder
	Then the folder is still there

Scenario: Simple large folder under git control can be deleted
	Given the folder contains 20 files
	And there is a .git subfolder
	When I ask the folder deleter if I am allowed to delete the folder
	And I ask the folder deleter to double-check this
	Then the result is yes
	And the double-check agrees

Scenario: Simple large folder ending with \obj can be deleted
	Given the folder contains 20 files
	And the folder ends with \obj
	When I ask the folder deleter if I am allowed to delete the folder
	And I ask the folder deleter to double-check this
	Then the result is yes
	And the double-check agrees

Scenario: Simple large folder beneath c:\temp can be deleted
	Given the folder contains 20 files
	And the folder is located beneath c:\temp
	When I ask the folder deleter if I am allowed to delete the folder
	And I ask the folder deleter to double-check this
	Then the result is yes
	And the double-check agrees