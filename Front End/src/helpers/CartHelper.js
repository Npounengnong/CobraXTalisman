//import React, { useState, useEffect } from 'react';
//import { useSelector, useDispatch } from 'react-redux';
import { showErrorMsg, showInfoMsg, showSuccessMsg } from './ValidationHelper';
//import rootAction from '../stateManagment/actions/rootAction';


export const AddProductToCart = (ProductId, 
    Quantity, productSelectedAttributes, DefaultImage) => {
    let cartItems = [];
    try {
      
       
        cartItems = JSON.parse(localStorage.getItem("cartItems"))
        cartItems = cartItems ?? [];

        //--check if product already exists
        if (cartItems?.filter(obj => obj.ProductId === ProductId).length > 0) {
            showInfoMsg("Product already exists in your cart!");
            return JSON.stringify(cartItems);
        } else {
            cartItems.push({
                ProductId: ProductId,
                productSelectedAttributes : productSelectedAttributes,
                Quantity: Quantity,
                ShippingCharges : 0,
                DefaultImage: DefaultImage
            });

            console.log(cartItems);

            debugger

            //--store in storage
            localStorage.setItem("cartItems", JSON.stringify(cartItems));

            showSuccessMsg("Added to the cart!");
              return JSON.stringify(cartItems);;
        }
    }
    catch (err) {
        console.log(err);
        showErrorMsg("An error occured. Please try again!");
          return JSON.stringify(cartItems);;
    }



};

export const AddCustomerWishList = (ProductId, ProductName  , Price,  DiscountedPrice  , DiscountId    ,
     IsDiscountCalculated, CouponCode   ,
     SizeId,  SizeShortName  , ColorId,  ColorName , Quantity, DefaultImage) => {
    let customerWishList = [];
    try {
        
       
        customerWishList = JSON.parse(localStorage.getItem("customerWishList"))
        customerWishList = customerWishList ?? [];

        //--check if product already exists
        if (customerWishList?.filter(obj => obj.ProductId === ProductId).length > 0) {
            showInfoMsg("Product already exists in your wish list!");
            return JSON.stringify(customerWishList);
        } else {
            customerWishList.push({
                ProductId: ProductId,
                ProductName : ProductName,
                Price: Price,
                DiscountedPrice : DiscountedPrice, 
                DiscountId :DiscountId   ,
                IsDiscountCalculated :IsDiscountCalculated, 
                CouponCode :CouponCode ,
                SizeId: SizeId,
                SizeShortName : SizeShortName,
                ColorId: ColorId,
                ColorName: ColorName,
                Quantity: Quantity,
                ShippingCharges: 0 ,
                DefaultImage: DefaultImage
            });

            console.log(customerWishList);

            showSuccessMsg("Added to your wish list!");
              return JSON.stringify(customerWishList);;
        }
    }
    catch (err) {
        console.log(err);
        showErrorMsg("An error occured. Please try again!");
          return JSON.stringify(customerWishList);;
    }



};