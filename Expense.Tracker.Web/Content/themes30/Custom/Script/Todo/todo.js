// todo item object
var todo = function (item) {
    var self = this;

    //observable Variables
    self.IsActive = ko.observable(false);
    self.ShowInClient = ko.observable(false);
    self.Title = ko.observable("");
    self.Description = ko.observable("");
    self.DueOn = ko.observable('');
    self.AppUserId = ko.observable("");
    self.AppUserName = ko.computed(function () { return searchAppUser(self.AppUserId()) });
    self.Mode = ko.observable("");
    self.slide = ko.observable("closed");
    self.TodoGroupId = ko.observable("");
    self.GroupName = ko.observable("");
    self.showAppUser = ko.observable(false);
    self.showDueDate = ko.observable(false);

    //toggle between slide open and slide closed
    self.toggle = function (data) {
        if (data.slide() == "opened")
            data.slide("closed");
        else
            data.slide("opened");
    }

    //Non-Observable variables
    self.oldTitle = "";
    self.oldDescription = "";
    self.oldDueOn = "";
    self.TodoId = "";
    self.ParentId = "";
    self.ParentType = "";
    self.oldActive = false;
    self.oldShowInClient = false;
    self.oldAppUserId = "";
    self.oldTodoGroupId = "";



    //assign values to observables and non observables
    if (item != null) {

        self.IsActive(item.IsActive);
        self.ShowInClient(item.ShowInClient);
        self.Title(item.Title);
        self.Description(item.Description);
        self.DueOn(formatDateTime(item.DueOn==null?'':item.DueOn));
        if (item.AppUserId == null) {
            item.AppUserId = 0;
        }
        self.AppUserId(item.AppUserId);
        self.TodoGroupId(item.TodoGroupId);
        self.GroupName(searchGroup(item.TodoGroupId));
        self.Mode(item.Mode);

        self.oldTitle = item.Title;
        self.oldDescription = item.Description;
        self.oldDueOn = item.DueOn;
        self.TodoId = item.TodoId;
        self.ParentId = item.ParentId;
        self.ParentType = item.ParentType;
        self.oldActive = item.IsActive;
        self.oldShowInClient = item.ShowInClient;
        self.oldAppUserId = item.AppUserId;
        self.oldTodoGroupId = item.TodoGroupId;
      //  self.currentContext = item.currentContext;
    }

    //validate if the item needs to be saved
    self.validate = function () {
        if (self.Title() === '') {
            alert("Please Enter the Title");
            return false;
        }
        return true;
    }

};


//formatting date time to dd-MM-yyyy HH:mm
function formatDateTime(dt) {
    if (dt != '') {
        var date = new Date(parseInt(dt.substring(dt.indexOf("(") + 1, dt.indexOf(")"))));
        var formatted = ("0" + date.getDate()).slice(-2) + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" +
            date.getFullYear() + " " + ("0" + date.getHours()).slice(-2) + ":" +
              ("0" + date.getMinutes()).slice(-2);
        return formatted;
    }
    return '';
}

//Search the AppuserName corresponding to AppUserId
function searchAppUser(id) {
    var ret = "";
    self.AppUserId = $.each(AppUsers, function (i, val) {
        if (val.AppUserId == id) {
            ret = val.AppUserName;
        }
    });
    return ret;
}

//Search GroupName Corresponding to the todoGroupId
function searchGroup(id) {
    var ret = id;
    self.TodoGroupId = $.each(Groups, function (i, val) {
        if (val.Id == id) {
            ret = val.Name;
        }
    });
    return ret;
}

//Main model containing array of todo objects
var mainModel = function () {
    var self = this;
    self.GName = ko.observable();
    self.GID = ko.observable();
    self.values = ko.observableArray([]);
    self.newToDo = ko.observable(false);
    self.GDescription = ko.observable("");
    var Message = ko.observable("");
    var Color = ko.observable("#000");
    self.items = ko.observable(0);

    self.newShowInClient = ko.observable(false);
    self.newTitle = ko.observable("");
    self.newDescription = ko.observable("");
    self.newAppUserId = ko.observable("");
    self.newAppUserName = ko.computed(function () { return searchAppUser(self.newAppUserId()) });
    self.newDueOn = ko.observable('');
    self.updateGroup = ko.observable(false);

    //search if any item exist for a particular group
    self.searchAny = function() {
        var flag = false;
        ko.utils.arrayForEach(self.values(), function (data) {
            if (self.GID() == data.TodoGroupId())
                flag = true;
        });
        return flag;
    }

    //toggle the Isactive button
    self.toggleActive = function (item) {
        self.Update(item);
        return true;
    },

    //Send ajax requests for creating new todo item
    self.CreateToDo = function () {
        var t = new todo();
        t.ShowInClient(self.newShowInClient());
        t.Title(self.newTitle());
        t.Description(self.newDescription());
        t.AppUserId(self.newAppUserId());
        t.TodoGroupId(self.GID());
        t.GroupName(self.GName());
        t.DueOn(self.newDueOn());

        if (t.validate()) {
            self.values.push(t);
            self.CancelModal(self);
        }
        ajaxRequest("POST", baseUrl + "ToDo/PostTodoItem/", t).done(function (data) {
            t.TodoId = data.TodoId;
            self.UpdateOldData(t);
            self.items(self.items() + 1);
        });
    },

    self.UpdateOldData = function(item){
        item.oldTitle = item.Title();
        item.oldDescription = item.Description();
        item.oldAppUserId = item.AppUserId();
        item.oldShowInClient = item.ShowInClient();
        item.oldAppUserId = item.AppUserId();
        item.oldDueOn = item.DueOn();
    }

    //storing old data when user wants to update todo
    self.Edit = function (item) {
        item.oldTitle = item.Title();
        item.oldDescription = item.Description();
        item.oldDueOn = item.DueOn();
        item.oldActive = item.IsActive();
        item.oldShowInClient = item.ShowInClient();
        item.oldAppUserId = item.AppUserId();
        item.Mode('edit');
    },

    //updating a todo item through ajax request
    self.Update = function (item) {

       // item.currentContext = self;
        var new_data = { "TodoId": item.TodoId, "TodoGroupId": self.GID(), "IsActive": item.IsActive(), "ShowInClient": item.ShowInClient(), "AppUserId": (item.AppUserId() == 0) ? null : item.AppUserId(), "TodoGroupId": item.TodoGroupId(), "Title": item.Title(), "Description": item.Description(), "DueOn": item.DueOn() };
        if(item.validate()) {
            ajaxRequest("PUT", baseUrl + "ToDo/UpdateTodoItem/", new_data).done(function (data) {
                item.slide("closed");
                self.UpdateOldData(item);
            }).error(function (err) {
                console.log(err);
            });
        }
        else {
            item.IsActive(item.oldActive);
            item.ShowInClient(item.oldShowInClient);
            item.AppUserId(item.oldAppUserId);
            item.Title(item.oldTitle);
            item.Description(item.oldDescription);
            item.DueOn(item.oldDueOn);
        }
        //item.AppUserName(searchAppUser(item.AppUserId()));
        Color("#0f0");
    },

    //updating group item through ajax request
    self.UpdateGroup = function (item) {
        var new_data = { "TodoGroupId": self.GID(), "parentId": parentId, "parentType": parentType, "Name": self.GName(), "Description": self.GDescription() }
        ajaxRequest("POST", baseUrl + "ToDo/UpdateGroup/", new_data).done(function (data) {
            self.CancelUpdate();
        }).error(function (e) {
            console.log(e);
        });
    },

    //cancel update modal for group
     self.CancelUpdate = function () {
         self.updateGroup(false);
     },

    //cancel modal for new todo item modal
    self.CancelModal = function (item) {

        self.newToDo(false);        
        item.newShowInClient(false);
        item.newAppUserId(0);
        item.newTitle("");
        item.newDescription("");
        item.newDueOn('');
    },

    //populate old data when cancel button pressed for editing todo
    self.Cancel = function (item) {
        item.slide("closed");
        item.IsActive(item.oldActive);
        item.ShowInClient(item.oldShowInClient);
        item.AppUserId(item.oldAppUserId);
        item.Title(item.oldTitle);
        item.Description(item.oldDescription);
        item.DueOn(item.oldDueOn);
    },

    //deleting a particular todo item through ajax request
    self.Delete = function (item) {
        ajaxRequest("DELETE", baseUrl + "ToDo/DeleteTodo/", item).done(function (data) {
            self.values.remove(item);
            self.refresh();
            self.items(self.items() - 1);
            Message("Item Deleted!!");
            Color("#f00");
        }).error(function (err) {
            console.log(err);
        });
    },

    //mapping the the todo items to their particular group
    self.Map = function (array) {
        ko.utils.arrayMap(array, function (item) {

            self.items(self.items()+1);
            item["Mode"] = "display";
            ////item["currentContext"] = self;
            self.values.push(new todo(item));
        });
    },

    //repopulate the items in a particluar group
    self.refresh = function () {

        var data = self.values().slice(0);
        self.values([]);
        self.values(data);
    }
}

//Strores all app users names and id
AppUsers = [{
    AppUserId: 0,
    AppUserName: "Select User"
}];

//Store all group elements
Groups = [];

//Initial Fetching appUsers, TodoGroups and its coprresponding todo items.
function init(func) {
    //Fetch AppUser
    ajaxRequest("GET", baseUrl + "ToDo/GetAssignedUsers/?parentId=" + parentId + "&parentType=" + parentType).done(function (data) {
        for (i = 0; i < data.length; i++)
                AppUsers.push(data[i]);
        //Fetch Group and todo elements
        ajaxRequest("GET", baseUrl + "ToDo/GetGroupedData/?parentId=" + parentId + "&parentType=" + parentType, "").done(function (data) {
        for (i = 0; i < data.length; i++) {           
            vm.AddGroup(data[i]);
        }
        
        if (func)
            func();
        }).error(function (d) {
            console.log(d);
        });

    }).error(function (d) {
        console.log(d);
    });
}

//Starting viewmodel
function viewModel() {
    var self = this;
    self.group = ko.observableArray([]);
    self.GID = ko.observable("");
    self.newGroupName = ko.observable("");
    self.newDescription = ko.observable("");
    self.newGroup = ko.observable(false);
    //self.isLoading = ko.observable(true);
   
    //Populating groups and values from data returned from ajax initially
    self.AddGroup = function (data) {
        var mm = new mainModel();
        mm.GID(data.TodoGroupId);
        mm.GName(data.Name);
        mm.GDescription(data.Description);

       // console.log(JSON.stringify(data.Todos));

        mm.Map(data.Todos);
        self.group.push(mm);
    }


    //creating new todo group
    self.CreateGroup = function () {
        var mm = new mainModel();
        var new_data = { "parentId": parentId, "parentType": parentType, "Name": self.newGroupName(), "Description": self.newDescription() };
        ajaxRequest("POST", baseUrl + "ToDo/PostGroup/", new_data).done(function (data) {
            mm.GID(data.TodoGroupId);
            mm.GName(data.Name);
            mm.GDescription(data.Description)
            mm.Map(data.Todos);
            self.group.push(mm);
            self.CancelModal();
        }).error(function (e) {
            console.log(e);
        });
    }

    //deleting a particular group
    self.DeleteGroup = function (data) {
        ko.utils.arrayForEach(self.group(), function (item) {
            if (data.GID() == item.GID()) {
                var new_data = { "parentId": parentId, "parentType": parentType, "Name": item.GName(), "Description": item.GDescription(), "TodoGroupId": item.GID() };
                ajaxRequest("DELETE", baseUrl + "ToDo/DeleteGroup/", new_data).done(function (data) {
                    self.group.remove(item);
                }).error(function (err) {
                    console.log(err); 
                });
            }
        });
        
    }

    //cancel new group modal
    self.CancelModal = function () {
        self.newGroup(false);
        self.newGroupName("");
        self.newDescription("");
    }
}

//the program starts from here
$(document).ready(function () {
    vm = new viewModel();
    init(function () {
        ko.applyBindings(vm);
    });
});


function delthis(obj)
{
    $(obj).closest(".content").hide();
}