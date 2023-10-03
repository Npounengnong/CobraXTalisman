import * as actionType from '../actionTypes';

const initialState = {
    // totalCartItems: 0,
    // cartItems : ""
};


const cartReducer = (state = initialState, action) => {
    switch (action.type) {
        case actionType.SET_CUSTOMER_CART:
            return {
                ...state,
                cartItems: action.payload

            }
        case actionType.EMPTY_CUSTOMER_CART:
            return {
                ...state,
                cartItems: []

            }

        case actionType.TOTAL_CART_ITEMS:
            return {
                ...state,
                totalCartItems: action.payload

            }

        case actionType.SET_CUSTOMER_WISHLIST:
            return {
                ...state,
                customerWishList: action.payload

            }
        default:
            return state
    }
}

export default cartReducer;