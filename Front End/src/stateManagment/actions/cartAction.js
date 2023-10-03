import * as actionType from '../actionTypes';

const setCustomerCart = (cart) => {
    return {
        type: actionType.SET_CUSTOMER_CART,
        payload: cart
    }
}

const emptyCustomerCart = (cart) => {
    return {
        type: actionType.EMPTY_CUSTOMER_CART,
        payload: cart
    }
}

const SetTotalCartItems = (qty) => {
    return {
        type: actionType.TOTAL_CART_ITEMS,
        payload: qty
    }
}

const setCustomerWishList = (wishlist) => {
    return {
        type: actionType.SET_CUSTOMER_WISHLIST,
        payload: wishlist
    }
}

export default {
    setCustomerCart,
    emptyCustomerCart,
    SetTotalCartItems,
    setCustomerWishList
}