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
        let messageBox = document.querySelector('.messageBox');
        messageBox.innerHTML = result.message;
        messageBox.style.display = "block";
        
        if (result.succes) {
            messageBox.classList.add('alert');
            messageBox.classList.add('alert-success');
            setTimeout(function () {
                messageBox.classList.remove('alert');
                messageBox.classList.remove('alert-success');
                messageBox.style.display = "none";
            }, 3000);
        } else {
            messageBox.classList.add('alert');
            messageBox.classList.add('alert-danger');
            setTimeout(function () {
                messageBox.classList.remove('alert');
                messageBox.classList.remove('alert-danger');
                messageBox.style.display = "none";
            }, 3000);
        }
    })
}

let addButtons = document.querySelectorAll('.addProduct');

addButtons.forEach(function (button) {
    button.addEventListener('click', function (event) {
        event.preventDefault();
        let formData = new FormData;
        formData.append('ProductId', button.parentElement.querySelector('input[name=ProductId]').value);
        formData.append('Quantity', button.parentElement.querySelector('input[name=Quantity]').value);
        
        Post(formData, '/Cart/AddItemToCart');

    })
})

function updateQuantity() {
    let updateButtons = document.querySelectorAll('.updateQuantity');
    updateButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            let formData = new FormData;
            formData.append('ProductId', button.parentElement.querySelector('input[name=ProductId]').value);
            formData.append('CartId', button.parentElement.querySelector('input[name=CartId]').value);
            formData.append('Quantity', button.parentElement.querySelector('input[name=Quantity]').value);
            
            Post(formData, '/Cart/ChangeQuantityInCart');
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
            formData.append('ProductId', button.parentElement.querySelector('input[name=ProductId]').value);
            formData.append('CartId', button.parentElement.querySelector('input[name=CartId]').value);

            Post(formData, '/Cart/DeleteFromCart');
            setTimeout(function () {
                location.reload();
            }, 3000);
           
        })
    })
}

deleteProduct();