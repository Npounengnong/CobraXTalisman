import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { MakeApiCallSynchronous, MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import { GetDefaultCurrencySymbol } from '../../../helpers/CommonHelper';

import Config from '../../../helpers/Config';


const ProductVariants = (props) => {

    const [ProductId, setProductId] = useState(props.ProductId);


    const styles = {
        popup: {
            display: props.showProductVariantsPopup ? "block" : "none",
            paddingRight: '16px'
        }
    };



    useEffect(() => {
        // declare the data fetching function
        const getProductAllAttributesById = async () => {

            const headersProdAttribte = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            const paramProdAttribute = {
                requestParameters: {
                    ProductId: ProductId,
                    recordValueJson: "[]",
                },
            };


            //--Get product all attributes by product id
            const responseProdAttributes = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_PRODUCT_ALL_ATTRIBUTES_BY_ID'], null, paramProdAttribute, headersProdAttribte, "POST", true);
            if (responseProdAttributes != null && responseProdAttributes.data != null) {

                //await setProductAllAttributes(JSON.parse(responseProdAttributes.data.data).filter(x=>x.AttributeDisplayName!="Colors" && x.AttributeDisplayName!="Size"));
                await props.setProductAllAttributes(JSON.parse(responseProdAttributes.data.data));
            }


        }


        // call the function
        getProductAllAttributesById().catch(console.error);


    }, [])


    return (
        <>
            <div
                className="modal fade sizeGuideModal show" style={styles.popup}
            >
                <div className="modal-dialog modal-dialog-centered" role="document">
                    <div className="modal-content">
                        <button type="button"
                            onClick={(e) => {
                                e.preventDefault();
                                props.closeProductVariantPopup();

                            }}



                            className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">
                                <i className="fas fa-times"></i>
                            </span>
                        </button>

                        <div className="modal-sizeguide">
                            <h3>Product Variants</h3>

                            <div className="text-align-left">
                                <ul className="list-group">




                                    {(() => {
                                        let attributeNames = props.productAllAttributes.map(({ ProductAttributeID, AttributeDisplayName }) => ({ ProductAttributeID, AttributeDisplayName }));
                                        attributeNames = attributeNames?.filter(x=>x.ProductAttributeID!= Config.PRODUCT_ATTRIBUTE_ENUM['Color'] && x.ProductAttributeID != Config.PRODUCT_ATTRIBUTE_ENUM['Size']);
                                        let uniqueAttributeNames = [...new Map(attributeNames.map((item) => [item["ProductAttributeID"], item])).values(),];
                                        return (
                                            uniqueAttributeNames?.map((atrItem, atrIdx) =>

                                                <li className="list-group-item">

                                                    <h3 className="product-variant-title">{atrItem.AttributeDisplayName}</h3>

                                                    {(() => {
                                                        let RowData = props.productAllAttributes?.filter(x => x.ProductAttributeID == atrItem.ProductAttributeID)
                                                        return (
                                                            RowData?.map((rowItem, rowIdx) =>
                                                                <div className="form-check form-check-inline">
                                                                    <input type="radio" 
                                                                    className="form-check-input" 
                                                                    name={rowItem.ProductAttributeID + rowItem.AttributeDisplayName} 
                                                                    id={rowItem.ProductAttributeID + rowItem.AttributeDisplayName + rowItem.PrimaryKeyValue}
                                                                    value = {rowItem.PrimaryKeyValue}
                                                                    onChange={(e) => props.setProductVariantsFromPopup(e.target.value, rowItem.ProductAttributeID )}
                                                                    />
                                                                    <label className="form-check-label" for={rowItem.ProductAttributeID + rowItem.AttributeDisplayName + rowItem.PrimaryKeyValue}>
                                                                        {rowItem.AdditionalPrice != undefined && rowItem.AdditionalPrice > 0 ?
                                                                         rowItem.PrimaryKeyDisplayValue + '\xa0\xa0' + " ( +" + GetDefaultCurrencySymbol() + rowItem.AdditionalPrice + ")"
                                                                        :
                                                                        rowItem.PrimaryKeyDisplayValue
                                                                        }
                                                                      
                                                                        </label>


                                                                </div>
                                                            )
                                                        );
                                                    })()}


                                                </li>
                                            )

                                        );


                                    })()}





                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default ProductVariants;