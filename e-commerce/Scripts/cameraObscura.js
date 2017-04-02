'use strict';

function Post(data, url) {
    fetch(url,
    {
        method: "POST",
        body: data,
        credentials: "same-origin",
    })
    .then(function (response) { return response.json(); })
    .then(function (result) {
      
        if (result.succes) {
            showSuccess(result.message);
            updateCart();
        } else {
            showError(result.message);
        }
    })
}

function showError(message) {
    let messageBox = document.querySelector('.messageBox');
    messageBox.innerHTML = message;
    messageBox.style.display = "block";

    messageBox.classList.add('alert');
    messageBox.classList.add('alert-danger');
    setTimeout(function () {
        messageBox.classList.remove('alert');
        messageBox.classList.remove('alert-danger');
        messageBox.style.display = "none";
    }, 3000);
}

function showSuccess(message) {
    let messageBox = document.querySelector('.messageBox');
    messageBox.innerHTML = message;
    messageBox.style.display = "block";

    messageBox.classList.add('alert');
    messageBox.classList.add('alert-success');
    setTimeout(function () {
        messageBox.classList.remove('alert');
        messageBox.classList.remove('alert-success');
        messageBox.style.display = "none";
    }, 3000);
}

let addButtons = document.querySelectorAll('.addProduct');

addButtons.forEach(function (button) {
    button.addEventListener('click', function (event) {
        event.preventDefault();
        let formData = new FormData;

        var productId = button.parentElement.querySelector('input[name=ProductId]').value;
        var qty = button.parentElement.querySelector('input[name=Quantity]').value;

        if ((productId != undefined && productId.length > 0) && (qty != undefined && qty > 0)) {
            formData.append('ProductId', productId);
            formData.append('Quantity', qty);
        
            Post(formData, '/Cart/AddItemToCart');
        } else {
            showError("Please provide a valid quantity.");
        }

    })
})

function AddOrder() {
    let OrderForm = document.querySelector('.order-form');
    if (OrderForm != null) {
        OrderForm.addEventListener('submit', function (event) {
            event.preventDefault();
            var Phone = OrderForm.querySelector('input[name=Phone]').value;
            if (Phone.match(/\d/g) && Phone.length == 10) {
                this.submit();
            } else {
                window.scrollTo(500, 0);

                showError("Invalid phonenumber.");
            }
        })
    }

}

AddOrder();

function updateCart() {
    let cartContainer = document.querySelector(".cart-container");
    fetch('/cart/Index', { credentials: "same-origin" })
    .then(function (response) { return response.text(); })
    .then(function (result) {
        
        cartContainer.innerHTML = result;
        updateQuantity();
        deleteProduct();
    })
}

function updateQuantity() {
    let updateButtons = document.querySelectorAll('.updateQuantity');
    updateButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            let formData = new FormData;

            var productId = button.parentElement.querySelector('input[name=ProductId]').value;
            var CartId = button.parentElement.querySelector('input[name=CartId]').value;
            var qty = button.parentElement.querySelector('input[name=Quantity]').value;

            if ((productId != undefined && productId.length > 0) && (CartId != undefined && CartId.length > 0) && (qty != undefined && qty > 0)) {
                formData.append('ProductId', productId);
                formData.append('CartId', CartId);
                formData.append('Quantity', qty);

                Post(formData, '/Cart/ChangeQuantityInCart');
            } else {
                showError("Please provide a valid quantity.");
            }

        })
    })
}

updateQuantity();

function deleteProduct() {
    let deleteButtons = document.querySelectorAll('.deleteProduct');
    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            let formData = new FormData;

            var productId = button.parentElement.querySelector('input[name=ProductId]').value;
            var CartId = button.parentElement.querySelector('input[name=CartId]').value;

            if ( (productId != undefined && productId.length > 0) && (CartId != undefined && CartId.length > 0) ) {
                formData.append('ProductId', productId);
                formData.append('CartId', CartId);

                Post(formData, '/Cart/DeleteFromCart');
             
            } else {
                showError("Please select a product from your cart.");
            }
 
        })
    })
}

deleteProduct();