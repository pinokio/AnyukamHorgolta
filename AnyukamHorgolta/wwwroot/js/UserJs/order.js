
var dataTable;

$(document).ready(function () {
    loadDataTable()
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Customer/Order/GetOrderList"
        },
        "columns": [
            { "data": "id", "width": "5%" },
            {
                "data": "orderDate", "render": function (data) {
                    var date = new Date(data);
                    return date.getFullYear() + "." + (date.getMonth() > 9 ? date.getMonth() : ("0" + date.getMonth())) + "." + (date.getDay() > 9 ? date.getDay() : ("0" + date.getDay()));
                }, "width": "10%"
            },
            { "data": "name", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderTotal", "render": function (data) { return data + " Ft" }, "width": "10%" },
            { "data": "orderStatus", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Customer/Order/Details/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                <i class="fas fa-edit"></i>
                            </a>
                        </div>
                     `;
                }, "width": "5%"
            }
        ]
    });
}
