List of class and member name changes for 2.0.0
=====================

Classes
---------------------

| Old Name | New Name |
| ------------- |-------------|
| GameTask | GameAction |
| GameTaskType  | GameActionType  |

Class members
---------------------

### Game

| Old Name | New Name |
| ------------- |-------------|
| Image | CoverImage |
| IsoPath | GameImagePath |
| Provider | PluginId |
| ProviderId | GameId |
| PlayTask | PlayAction |
| OtherTasks | OtherActions |

### GameAction

| Old Name | New Name |
| ------------- |-------------|
| GameTaskType  | GameActionType  |
| GameTaskType  | GameActionType  |
| IsPrimary  | *removed*  |

### IPlayniteAPI

| Old Name | New Name |
| ------------- |-------------|
| ResolveGameVariables | ExpandGameVariables |

Type changes
---------------------

### Game

| Member | Old Type | New Type | 
| ------------- |-------------|-------------|
| PlatformId | ObjectId  | Guid |

### Emulator

| Member | Old Type | New Type | 
| ------------- |-------------|-------------|
| Id | ObjectId  | Guid |

### EmulatorProfile 

| Member | Old Type | New Type | 
| ------------- |-------------|-------------|
| Id | ObjectId  | Guid |
| Platforms | List<ObjectId> | List<Guid> |

### Platform

| Member | Old Type | New Type | 
| ------------- |-------------|-------------|
| Id | ObjectId  | Guid |

### GameAction

| Member | Old Type | New Type | 
| ------------- |-------------|-------------|
| EmulatorId | ObjectId  | Guid |
| EmulatorProfileId | ObjectId  | Guid |