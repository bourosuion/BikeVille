export interface Product {
    id: number;
    title: string;
    price: number;
    description?: string; // Optional because it may not be present everywhere
    image?: string; // Optional because it may not be present everywhere
    quantity: number;
    removing: boolean;
    isWishlisted: boolean;
}
